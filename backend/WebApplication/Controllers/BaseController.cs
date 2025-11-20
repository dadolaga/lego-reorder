using Microsoft.AspNetCore.Mvc;
using System.Net;
using WebApplication.Models;

namespace WebApplication.Controllers {
    public class BaseController : ControllerBase {
        protected static int OK = 0;
        protected static int ELEMENT_NOT_FOUND = 10;
        protected static int ELEMENT_ALREADY_EXIST = 11;
        protected static int GENERIC_DB_ERROR = 100;
        protected static int INSERT_DB_ERROR = 101;

        protected IActionResult CreateSuccessResponse<T>(T data, string message = "Success function") {
            var response = new Response<T> {
                Code = OK,
                Message = message,
                Data = data
            };

            return StatusCode(200, response);
        }
        protected IActionResult CreateSuccessListResponse<T>(IEnumerable<T> data, int count, string message = "Success function") {
            var response = new Response<IEnumerable<T>> {
                Code = OK,
                Message = message,
                Count = count,
                Data = data
            };

            return StatusCode(200, response);
        }

        protected IActionResult CreateSuccessEmptyResponse() {
            return StatusCode(204);
        }

        protected IActionResult CreateFailResponse(int code, string message = "Fail to execution that function") {
            var response = new Response {
                Code = code,
                Message = message,
                Data = null
            };

            return StatusCode(400, response);
        }

        protected IActionResult CreateFailResponse<T>(int code, string message, T data) {
            var response = new Response<T> {
                Code = code,
                Message = message,
                Data = data
            };

            return StatusCode(400, response);
        }
    }
}
