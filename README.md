# AzureAuth
This is a experiment into authentication and authorization using JWTs and microservices designed for Azure.

The projects are setup to use HTTPS, so please check the "Certificate" solution folder, or the certificate.readme file for further instructions to get this solution to run.

## Session service
The session service stores the JWT claims in a table storage. They can be retrieved using a guid. The intended audience are both automated services and normal users, the main difference being that what is using the sessions (a browser, in the case of users).

Users therefore has a slightly more secure flow, where refresh tokens are stored in httponly secure cookies, and the id is recycled every time a session is refreshed. This means the auth id will only be transmitted twice at most.

To start a session, the user must get a valid JWT from an authorizer.

## Developer authorizer
Authorizers exchange credentials for JWTs. The developer authorizer only requires a username and is of course not intended for production scenarios.

## Reference API
The reference API is the last piece of this experiment. It only has one endpoint, /autorized, which returns the username of the caller. 

It also has a index.html page which can be used to test building JWTs and user auth flows.