import { Directive, HostListener, Output, EventEmitter, Input, ElementRef, Renderer2, OnInit, Injectable } from '@angular/core';

@Directive({ 
  selector: '[appBlumeDrop]'
})
  
export class BlumeDropDirective {
  @Output() filesDropped = new EventEmitter<File[]>();
  @Input() allowedFileTypes: string[] = [];
  @Input() uploadUrl: string = '';
  @Input() allowMultiSelect: boolean = true;
  private fileInput: HTMLInputElement = new HTMLInputElement();

  constructor(private elementRef: ElementRef, private renderer: Renderer2) { }

  ngOnInit() {
    // create the file input element
    this.fileInput = this.renderer.createElement('input');
    this.renderer.setProperty(this.fileInput, 'type', 'file');
    this.renderer.setStyle(this.fileInput, 'display', 'none');
    this.renderer.listen(this.fileInput, 'change', (event) => this.handleInputChange(event));
    // append the file input element to the host element
    this.renderer.appendChild(this.elementRef.nativeElement, this.fileInput);
  }

  @HostListener('dragover', ['$event']) onDragOver(evt: DragEvent) {
    evt.preventDefault();
    evt.stopPropagation();
  }

  @HostListener('dragleave', ['$event']) public onDragLeave(evt: DragEvent) {
    evt.preventDefault();
    evt.stopPropagation();
  }

  @HostListener('drop', ['$event']) public onDrop(evt: DragEvent) {
    evt.preventDefault();
    evt.stopPropagation();

    if (evt.dataTransfer == null) throw new Error('Error: No data transfered during the drop event.');

    let files: Array<File> = [];
    files = Array.from(evt.dataTransfer.files);
    if (!this.allowMultiSelect) {
      files = [files[0]]
    }

    if (files.length > 0) {
      this.handleFiles(files);
    }
  }

  @HostListener('click') public onClick() {
    this.fileInput.click();
  }

  private handleInputChange(event: any) {
    let files: Array<File> = event.target.files;

    if (!this.allowMultiSelect) {
      files = [files[0]]
    }
    
    if (files.length > 0) {
      this.handleFiles(files);
    }
  }

  private handleFiles(files: File[]) {
    // filter out files that are not of the allowed types
    let allowedFiles: File[] = files.filter(file => this.allowedFileTypes.includes(file.type));
    if (allowedFiles.length > 0) {
      this.filesDropped.emit(allowedFiles);
    }
  }
}