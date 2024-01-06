# AzureAuth
This is a experiment into authentication and authorization using [JWTs](https://jwt.io/) and microservices designed for Azure.

The projects are setup to use HTTPS, so please check the [certificate.readme](certificate.readme) file for further instructions to get this solution to run.
AzureAuth is an experiment that explores authentication and authorization using JSON Web Tokens (JWTs) and microservices specifically designed for Azure environments.

## Session service
The session service's primary goal is to securely store claim sets and promote proper credential management. A claim set is linked to a unique identifier called AuthID or refresh token. It is agnostic of the claims, and could be configured to encrypt them should they contain PII.

The intended audiences are both automated services and normal users.

Services send a custom http header containing the AuthID, and the reply is a valid JWT. These requests should ideally be in an intranet. Service claim sets are only deleted when their expiration date is reached.

User requests comes from a browser and measures must be taken to protect the data against xss.
The auth id is therefore protected as a httponly secure cookie, and it changes every time the JWT is refreshed.

The included sample JS client uses [private fields](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Guide/Using_Classes#private_fields) to store the JWT. Even so, the session service does not accept itself as an authorizer, so a leaked JWT cannot be used to create a new session.

The default lifetime of a refresh token is 7 days, and the default lifetime of a JWT is 15 minutes. 

To start a session, a service must have their claims object created by an administrator and directly inserted into table storage. The jwt lifetime can also be set there, since it depends on the needs of the service.

The user must get a valid JWT from an authorizer service. This JWT should be disposed of, as soon as the session been established.

## Developer authorizer
Authorizers exchange credentials for JWTs. The developer authorizer only requires a username and is of course not intended for production scenarios.

## Reference API
The final part of this experiment is the reference API, featuring an endpoint /authorized that returns the caller's username. Additionally, it includes an index.html page that can be used to test JWT construction and user authentication workflows. To enable this "frontend", the session service's Cross-Origin Resource Sharing (CORS) settings have been adjusted accordingly.