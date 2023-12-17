# AzureAuth
This is a experiment into authentication and authorization using [JWTs](https://jwt.io/) and microservices designed for Azure.

The projects are setup to use HTTPS, so please check the certificate.readme file for further instructions to get this solution to run.

## Session service
The session service stores the JWT claims in a table storage. They can be retrieved using a guid. 
The intended audience are both automated services and normal users.
Service requests comes from custom clients, who send their auth id in a custom header. User requests comes from a browser, and refresh tokens are protected as httponly secure cookies. 
The id of user sessions are changed (the claims are recycled) for every refresh, while service sessions are deleted only when their expiration date is reached.

To start a session, a service must have their claims object created by an administrator and directly inserted into table storage.
The user must get a valid JWT from an authorizer.

## Developer authorizer
Authorizers exchange credentials for JWTs. The developer authorizer only requires a username and is of course not intended for production scenarios.

## Reference API
The reference API is the last piece of this experiment. It only has one endpoint, /authorized, which returns the username of the caller. 

It also has a index.html page which can be used to test building JWTs and user auth flows.