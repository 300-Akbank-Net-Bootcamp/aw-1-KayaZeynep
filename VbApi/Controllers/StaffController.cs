using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace VbApi.Controllers;

public class Staff
{
    [Required]
    [StringLength(maximumLength: 250, MinimumLength = 10)]
    public string? Name { get; set; }

    [EmailAddress(ErrorMessage = "Email address is not valid.")]
    public string? Email { get; set; }

    [Phone(ErrorMessage = "Phone is not valid.")]
    public string? Phone { get; set; }

    [Range(minimum: 30, maximum: 400, ErrorMessage = "Hourly salary does not fall within allowed range.")]
    public decimal? HourlySalary { get; set; }
}

public class StaffValidator : AbstractValidator<Staff>
{
    public StaffValidator()
    {
        RuleFor(x => x.Name).Length(10, 250).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().WithMessage("Email address is not valid.");
        RuleFor(x => x.Phone).NotEmpty().NotNull().WithMessage("Phone is not valid.").MinimumLength(10)
            .WithMessage("PhoneNumber must not be less than 10 characters.")
            .MaximumLength(20).WithMessage("PhoneNumber must not exceed 50 characters.")
            .Matches(new Regex(@"((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}")).WithMessage("PhoneNumber not valid");
        RuleFor(e => e.HourlySalary).InclusiveBetween(30, 400).WithMessage("Hourly salary does not fall within allowed range.");
    } 
}

[Route("api/[controller]")]
[ApiController]
public class StaffController : ControllerBase
{
    public StaffController()
    {
    }

    [HttpPost]
    public Staff Post([FromBody] Staff value)
    {
        StaffValidator validator = new StaffValidator();
        var result = validator.Validate(value);
        
        if(! result.IsValid)
        {
            throw new Exception(result.Errors.ToString());
        }
        return value;
    }
}