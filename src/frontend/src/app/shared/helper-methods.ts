import { Injectable } from '@angular/core';

@Injectable({
    providedIn: 'root'
})
export class HelperMethods {
    constructor() { }
    
    public static toNumber(value: number | undefined | null): number {
        if (value == null || value == undefined) {
            return 0;
        }
        
        return value;
    }
}