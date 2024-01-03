# AzureAuth
This is a experiment into authentication and authorization using [JWTs](https://jwt.io/) and microservices designed for Azure.

The projects are setup to use HTTPS, so please check the [certificate.readme](certificate.readme) file for further instructions to get this solution to run.

## Session service
The session service is agnostic regarding the claims it stores in a table storage. Its main purpose is the (hopefully) safe storage of sets of claims, and to encourage proper handling of credentials. A claim set is associated with a guid called AuthID or refresh token.

The intended audiences are both automated services and normal users.

Service requests comes from custom clients, who send their auth id in a custom header. Service claim sets are only deleted when their expiration date is reached.

User requests comes from a browser, and auth id are protected as httponly secure cookies. 
The auth id of user sessions are changed for every refresh of the JWTs, and are sometimes called refresh tokens.

The default lifetime of a refresh token is 7 days, and the default lifetime of a JWT is 15 minutes. Remember to protect them apropriately if you make your own js client.
(The included sample JS client uses [private fields](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Using_Classes#private_fields) to store the JWT, and it cannot access the auth id at all, since it is httponly. Even so, the session service does not accept itself as an authorizer, so a leaked JWT cannot be used to create a new session.)

To start a session, a service must have their claims object created by an administrator and directly inserted into table storage. The jwt lifetime can also be set there.

The user must get a valid JWT from an authorizer service.

## Developer authorizer
Authorizers exchange credentials for JWTs. The developer authorizer only requires a username and is of course not intended for production scenarios.

## Reference API
The reference API is the last piece of this experiment. It has an endpoint, /authorized, which returns the username of the caller. 

It also has a index.html page which can be used to test building JWTs and user auth flows. The session service CORS are set to allow this "frontend".