# Mini Appointment Booking System


# Initialization

Guide you on preparing the environemnt and install necessary tools and softwares

## Install RabbitMQ
Pull the docker image

    docker pull rabbitmq:3-management

Run docker container

    docker run -d  --hostname RabbitMQ  --name RabbitMQ  -p 5672:5672  -p 15672:15672  rabbitmq:3-management

Docker will be running the **Web Portal** in ***http://localhost:15672/***
User name : guest
Password : guest
TCP Port : 5672

## System Architecture

```mermaid
graph TD;
    BlueprintX --> DB(Database);    
    Angular -- Rest API --> WebAPI;
    WebAPI -- Business Logic --> S{Services};
    S -- View Model + Entity --> Data;
    Data --> BlueprintX;
    S --> P(RabbitMQ Producer)
    P --> RabbitMQ;
    RC(Rabbit Consumer) --> DB;
    RabbitMQ --> RC;
```


## System Architecture Proposal

```mermaid
flowchart  LR;
	A["Admin"]  -- HTTP -->  C["Create/Update API"]  &  G["Get API"]  &  P["Payment Gateway API"]
	C  -->  DS["Dedicated Scheduler<br>For<br>Notification Management"]
	DS  -->  NQ["Notification Que"]
	NQ  -->  SMTP
	NQ  -->  Twillo
	C  -->  Q[("Messaging Que")]
	Q  -- Creates/Updates -->  DB[("Database")]
	Q  -- Updates -->  CS[("Redis")]
	G  -- Check -->  CS
	G  -- If No Cache -->  DB
	G  -- Update -->  CS
	P  -->  PQ[("Payment Que")]
	PQ  -->  PG["Payment Gateway"]  &  PM["SMTP"]  &  PT["Twillo"]
```

## Test Data

Login
{
  "email": "nafis.sadik13@yahoo.com",
  "password": "a8i#YJVFzk9N"
}

Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySWQiOiIxIiwiVXNlck5hbWUiOiJuYWZpc19zYWRpayIsImV4cCI6MTc1MTQwODg5OSwiaXNzIjoiQmx1ZXByaW50WCIsImF1ZCI6IlVzZXIifQ.I8oEtc6EgNW4w1IMor_-MRgMc5Lym0BUlK1FZjBRskc

clinic - post
{
  "clinicId": 0,
  "name": "Popular Diagnostics - Uttara",
  "address": "Jashina Road, Uttara, Dhaka",
  "contactNumber": "0123456789",
  "operatingHours": "09:00:00",
  "closingHours": "15:16:17"
}

doctor - post
{
  "doctorId": 0,
  "clinicId": 1,
  "name": "Dr. John",
  "specialization": "Sergon",
  "contactInformation": "1234"
}

{
  "scheduleId": 0,
  "doctorId": 4,
  "dayOfWeek": "SUN",
  "startTime": "07:00:00",
  "endTime": "15:00:00",
  "visitTimeSapn": "00:30:00"
}

{
  "id": 0,
  "name": "Mr. Smith",
  "contactInformation": "12132344"
}
