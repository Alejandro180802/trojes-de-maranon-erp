using Microsoft.AspNetCore.Mvc;
using TrojesDeMaranon.Application.Common;

namespace TrojesDeMaranon.Api.Controllers;

internal static class ControllerResponseExtensions
{
    public static ActionResult<ApiResponse<T>> ToActionResult<T>(this ControllerBase controller, ApiResponse<T> response)
    {
        if (response.Success)
        {
            return controller.Ok(response);
        }

        return IsNotFound(response) ? controller.NotFound(response) : controller.BadRequest(response);
    }

    public static ActionResult<ApiResponse<T>> ToCreatedAtActionResult<T>(
        this ControllerBase controller,
        ApiResponse<T> response,
        string actionName,
        object routeValues)
    {
        if (response.Success)
        {
            return controller.CreatedAtAction(actionName, routeValues, response);
        }

        return IsNotFound(response) ? controller.NotFound(response) : controller.BadRequest(response);
    }

    private static bool IsNotFound<T>(ApiResponse<T> response) =>
        response.Errors.Any(error => error.Contains("no encontr", StringComparison.OrdinalIgnoreCase));
}
