using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace EShopWeb.Controllers
{
    public class ErrorController : Controller
    {       
        public ErrorController()
        {
            
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }

        [Route("/Error/Error")]
        public IActionResult Error([FromQuery]HttpStatusCode StatusCode)
        {
            Exception exec = (Exception)HttpContext.Items["exception"];
            ViewData["message"] = exec.Message; 
            int statuscode = (int)StatusCode;  
            string description = StatusCode switch
            {
                HttpStatusCode.Continue => "The server has received the request headers and the client should proceed to send the request body (in the case of a request for which a body needs to be sent; for example, a POST request). Sending a large request body to a server after a request has been rejected for inappropriate headers would be inefficient. To have a server check the request's headers, a client must send Expect: 100-continue as a header in its initial request and receive a 100 Continue status code in response before sending the body. If the client receives an error code such as 403 (Forbidden) or 405 (Method Not Allowed) then it should not send the request's body. The response 417 Expectation Failed indicates that the request should be repeated without the Expect header as it indicates that the server does not support expectations (this is the case, for example, of HTTP/1.0 servers)",
                HttpStatusCode.SwitchingProtocols => "The requester has asked the server to switch protocols and the server has agreed to do so.",
                HttpStatusCode.Processing => "A WebDAV request may contain many sub-requests involving file operations, requiring a long time to complete the request. This code indicates that the server has received and is processing the request, but no response is available yet.[3] This prevents the client from timing out and assuming the request was lost. The status code is deprecated.",
                HttpStatusCode.EarlyHints => "Used to return some response headers before final HTTP message",
                HttpStatusCode.OK => "Standard response for successful HTTP requests. The actual response will depend on the request method used. In a GET request, the response will contain an entity corresponding to the requested resource. In a POST request, the response will contain an entity describing or containing the result of the action.",
                HttpStatusCode.Created => "The request has been fulfilled, resulting in the creation of a new resource.",
                HttpStatusCode.Accepted => "The request has been accepted for processing, but the processing has not been completed. The request might or might not be eventually acted upon, and may be disallowed when processing occurs.",
                HttpStatusCode.NonAuthoritativeInformation => "The server is a transforming proxy (e.g. a Web accelerator) that received a 200 OK from its origin, but is returning a modified version of the origin's response.",
                HttpStatusCode.NoContent => "The server successfully processed the request, and is not returning any content.",
                HttpStatusCode.ResetContent => "Tells the user agent to reset the document which sent this request.",
                HttpStatusCode.PartialContent => "This response code is used when the Range header is sent from the client to request only part of a resource.",
                HttpStatusCode.MultiStatus => "Conveys information about multiple resources, for situations where multiple status codes might be appropriate.",
                HttpStatusCode.AlreadyReported => "Used inside a <dav:propstat> response element to avoid repeatedly enumerating the internal members of multiple bindings to the same collection.",
                HttpStatusCode.IMUsed => "The server has fulfilled a GET request for the resource, and the response is a representation of the result of one or more instance-manipulations applied to the current instance.",
                HttpStatusCode.MultipleChoices => "The request has more than one possible response. The user agent or user should choose one of them. (There is no standardized way of choosing one of the responses, but HTML links to the possibilities are recommended so the user can pick.)",
                HttpStatusCode.Moved => "The URL of the requested resource has been changed permanently. The new URL is given in the response.",
                //case HttpStatusCode.MovedPermanently:
                //    break;
                HttpStatusCode.Found => "This response code means that the URI of requested resource has been changed temporarily. Further changes in the URI might be made in the future. Therefore, this same URI should be used by the client in future requests.",
                //case HttpStatusCode.Redirect:
                //    break;
                //case HttpStatusCode.RedirectMethod:
                //    description = "";
                //    break;
                HttpStatusCode.SeeOther => "The server sent this response to direct the client to get the requested resource at another URI with a GET request.",
                HttpStatusCode.NotModified => "Indicates that the resource has not been modified since the version specified by the request headers If-Modified-Since or If-None-Match. In such case, there is no need to retransmit the resource since the client still has a previously-downloaded copy.",
                HttpStatusCode.UseProxy => "Defined in a previous version of the HTTP specification to indicate that a requested response must be accessed by a proxy. It has been deprecated due to security concerns regarding in-band configuration of a proxy.",
                HttpStatusCode.Unused => "This response code is no longer used; it is just reserved. It was used in a previous version of the HTTP/1.1 specification.",
                //case HttpStatusCode.RedirectKeepVerb:
                //    description = "";
                //    break;
                HttpStatusCode.TemporaryRedirect => "The server sends this response to direct the client to get the requested resource at another URI with the same method that was used in the prior request. This has the same semantics as the 302 Found HTTP response code, with the exception that the user agent must not change the HTTP method used: if a POST was used in the first request, a POST must be used in the second request.",
                HttpStatusCode.PermanentRedirect => "This means that the resource is now permanently located at another URI, specified by the wareHouse: HTTP Response header. This has the same semantics as the 301 Moved Permanently HTTP response code, with the exception that the user agent must not change the HTTP method used: if a POST was used in the first request, a POST must be used in the second request.",
                HttpStatusCode.BadRequest => "The server cannot or will not process the request due to something that is perceived to be a client error (e.g., malformed request syntax, invalid request message framing, or deceptive request routing).",
                HttpStatusCode.Unauthorized => @"Similar to 403 Forbidden, but specifically for use when authentication is required and has failed or has not yet been provided. The response must include a WWW-Authenticate header field containing a challenge applicable to the requested resource. See Basic access authentication and Digest access authentication. 401 semantically means 'unauthorised', the user does not have valid authentication credentials for the target resource. Some sites incorrectly issue HTTP 401 when an IP address is banned from the website(usually the website domain) and that specific address is refused permission to access a website.",
                HttpStatusCode.PaymentRequired => "Reserved for future use. The original intention was that this code might be used as part of some form of digital cash or micropayment scheme, as proposed, for example, by GNU Taler,[14] but that has not yet happened, and this code is not widely used. Google Developers API uses this status if a particular developer has exceeded the daily limit on requests.[15] Sipgate uses this code if an account does not have sufficient funds to start a call.[16] Shopify uses this code when the store has not paid their fees and is temporarily disabled.[17] Stripe uses this code for failed payments where parameters were correct, for example blocked fraudulent payments.",
                HttpStatusCode.Forbidden => "The request contained valid data and was understood by the server, but the server is refusing action. This may be due to the user not having the necessary permissions for a resource or needing an account of some sort, or attempting a prohibited action (e.g. creating a duplicate record where only one is allowed). This code is also typically used if the request provided authentication by answering the WWW-Authenticate header field challenge, but the server did not accept that authentication. The request should not be repeated.",
                HttpStatusCode.NotFound => "The requested resource could not be found but may be available in the future. Subsequent requests by the client are permissible.",
                HttpStatusCode.MethodNotAllowed => "A request method is not supported for the requested resource; for example, a GET request on a form that requires data to be presented via POST, or a PUT request on a read-only resource.",
                HttpStatusCode.NotAcceptable => "The requested resource is capable of generating only content not acceptable according to the Accept headers sent in the request. See Content negotiation.",
                HttpStatusCode.ProxyAuthenticationRequired => "This is similar to 401 Unauthorized but authentication is needed to be done by a proxy.",
                HttpStatusCode.RequestTimeout => "This response is sent on an idle connection by some servers, even without any previous request by the client. It means that the server would like to shut down this unused connection. This response is used much more since some browsers, like Chrome, Firefox 27+, or IE9, use HTTP pre-connection mechanisms to speed up surfing. Also note that some servers merely shut down the connection without sending this message.",
                HttpStatusCode.Conflict => "This response is sent when a request conflicts with the current state of the server.",
                HttpStatusCode.Gone => "This response is sent when the requested content has been permanently deleted from server, with no forwarding address. Clients are expected to remove their caches and links to the resource. The HTTP specification intends this status code to be used for 'limited - time, promotional services'. APIs should not feel compelled to indicate resources that have been deleted with this status code.",
                HttpStatusCode.LengthRequired => "Server rejected the request because the Content-Length header field is not defined and the server requires it.",
                HttpStatusCode.PreconditionFailed => "The client has indicated preconditions in its headers which the server does not meet.",
                HttpStatusCode.RequestEntityTooLarge => "Request entity is larger than limits defined by server. The server might close the connection or return an Retry-After header field.",
                HttpStatusCode.RequestUriTooLong => "The URI requested by the client is longer than the server is willing to interpret.",
                HttpStatusCode.UnsupportedMediaType => "The media format of the requested data is not supported by the server, so the server is rejecting the request.",
                HttpStatusCode.RequestedRangeNotSatisfiable => "The range specified by the Range header field in the request cannot be fulfilled. It's possible that the range is outside the size of the target URI's data.",
                HttpStatusCode.ExpectationFailed => "This response code means the expectation indicated by the Expect request header field cannot be met by the server.",
                HttpStatusCode.MisdirectedRequest => "The request was directed at a server that is not able to produce a response. This can be sent by a server that is not configured to produce responses for the combination of scheme and authority that are included in the request URI.",
                HttpStatusCode.UnprocessableEntity => "The request was well-formed but was unable to be followed due to semantic errors.",
                HttpStatusCode.Locked => "The resource that is being accessed is locked.",
                HttpStatusCode.FailedDependency => "The request failed due to failure of a previous request.",
                HttpStatusCode.UpgradeRequired => "The server refuses to perform the request using the current protocol but might be willing to do so after the client upgrades to a different protocol. The server sends an Upgrade header in a 426 response to indicate the required protocol(s).",
                HttpStatusCode.PreconditionRequired => "The origin server requires the request to be conditional. This response is intended to prevent the 'lost update' problem, where a client GETs a resource's state, modifies it and PUTs it back to the server, when meanwhile a third party has modified the state on the server, leading to a conflict.",
                HttpStatusCode.TooManyRequests => "The user has sent too many requests in a given amount of time ('rate limiting').",
                HttpStatusCode.RequestHeaderFieldsTooLarge => "The server is unwilling to process the request because its header fields are too large. The request may be resubmitted after reducing the size of the request header fields.",
                HttpStatusCode.UnavailableForLegalReasons => "The user agent requested a resource that cannot legally be provided, such as a web page censored by a government.",
                HttpStatusCode.InternalServerError => "The server has encountered a situation it does not know how to handle.",
                HttpStatusCode.NotImplemented => "The request method is not supported by the server and cannot be handled. The only methods that servers are required to support (and therefore that must not return this code) are GET and HEAD.",
                HttpStatusCode.BadGateway => "This error response means that the server, while working as a gateway to get a response needed to handle the request, got an invalid response.",
                HttpStatusCode.ServiceUnavailable => "The server is not ready to handle the request. Common causes are a server that is down for maintenance or that is overloaded. Note that together with this response, a user-friendly page explaining the problem should be sent. This response should be used for temporary conditions and the Retry-After HTTP header should, if possible, contain the estimated time before the recovery of the service. The webmaster must also take care about the caching-related headers that are sent along with this response, as these temporary condition responses should usually not be cached.",
                HttpStatusCode.GatewayTimeout => "This error response is given when the server is acting as a gateway and cannot get a response in time.",
                HttpStatusCode.HttpVersionNotSupported => "The HTTP version used in the request is not supported by the server.",
                HttpStatusCode.VariantAlsoNegotiates => "The server has an internal configuration error: the chosen variant resource is configured to engage in transparent content negotiation itself, and is therefore not a proper end point in the negotiation process.",
                HttpStatusCode.InsufficientStorage => "The method could not be performed on the resource because the server is unable to store the representation needed to successfully complete the request.",
                HttpStatusCode.LoopDetected => "The server detected an infinite loop while processing the request.",
                HttpStatusCode.NotExtended => "Further extensions to the request are required for the server to fulfill it.",
                HttpStatusCode.NetworkAuthenticationRequired => "Indicates that the client needs to authenticate to gain network access.",
                _ => "unknown",
            };
            ViewData["statuscode"] = statuscode;
            ViewData["description"] = description;
            return View("Error");
        }

        public ActionResult AccessDenied()
        {
            return View();
        }
    }

    public class NotFoundViewResult : ViewResult
    {
        public NotFoundViewResult(string viewName)
        {
            ViewName = viewName;            
        }
    }

    public class HttpStatusCodeWithBodyResult : ViewResult
    { 
        public HttpStatusCodeWithBodyResult(int statusCode,string message)
        {          
            ViewName = "~/Views/Shared/StatusCodeErrorView.cshtml";
        }         
    }
}