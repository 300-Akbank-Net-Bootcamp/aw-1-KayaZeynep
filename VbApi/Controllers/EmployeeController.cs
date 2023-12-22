using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace VbApi.Controllers;

public class Employee : IValidatableObject
{
    [Required]
    [StringLength(maximumLength: 250, MinimumLength = 10, ErrorMessage = "Invalid Name")]
    public string Name { get; set; }

    [Required] 
    public DateTime DateOfBirth { get; set; }

    [EmailAddress(ErrorMessage = "Email address is not valid.")]
    public string Email { get; set; }

    [Phone(ErrorMessage = "Phone is not valid.")]
    public string Phone { get; set; }

    [Range(minimum: 50, maximum: 400, ErrorMessage = "Hourly salary does not fall within allowed range.")]
    [MinLegalSalaryRequired(minJuniorSalary: 50, minSeniorSalary: 200)]
    public double HourlySalary { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var minAllowedBirthDate = DateTime.Today.AddYears(-65);
        if (minAllowedBirthDate > DateOfBirth)
        {
            yield return new ValidationResult("Birthdate is not valid.");
        }
    }
}

public class EmployeeValidator : AbstractValidator<Employee>
{
    public EmployeeValidator()
    {
        RuleFor(x => x.Name).Length(10, 250).NotEmpty();
        RuleFor(x => x.DateOfBirth).NotEmpty();
        RuleFor(x => x.Email).EmailAddress().WithMessage("Email address is not valid.");
        RuleFor(x => x.Phone).NotEmpty().NotNull().WithMessage("Phone is not valid.").MinimumLength(10)
            .WithMessage("PhoneNumber must not be less than 10 characters.")
            .MaximumLength(20).WithMessage("PhoneNumber must not exceed 50 characters.")
            .Matches(new Regex(@"((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}")).WithMessage("PhoneNumber not valid");
        RuleFor(e => e.HourlySalary).InclusiveBetween(50,400)
            .Must((e, hourlySalary) => IsHourlySalaryValid(e.DateOfBirth, hourlySalary, 50, 200))
            .WithMessage("Hourly salary does not fall within allowed range.");


    }
    private bool IsHourlySalaryValid(DateTime birth, double salary, double minSeniorSalary, double minJuniorSalary)
    {
        var dateBeforeThirtyYears = DateTime.Today.AddYears(-30);
        var isOlderThanThirdyYears = birth <= dateBeforeThirtyYears;

        var isValidSalary = isOlderThanThirdyYears ? salary >= minSeniorSalary : salary >= minJuniorSalary;

        return isValidSalary;
    }
}


public class MinLegalSalaryRequiredAttribute : ValidationAttribute
{
    public MinLegalSalaryRequiredAttribute(double minJuniorSalary, double minSeniorSalary)
    {
        MinJuniorSalary = minJuniorSalary;
        MinSeniorSalary = minSeniorSalary;
    }

    public double MinJuniorSalary { get; }
    public double MinSeniorSalary { get; }
    public string GetErrorMessage() => $"Minimum hourly salary is not valid.";

    protected override ValidationResult? IsValid(object value, ValidationContext validationContext)
    {
        var employee = (Employee)validationContext.ObjectInstance;
        var dateBeforeThirtyYears = DateTime.Today.AddYears(-30);
        var isOlderThanThirdyYears = employee.DateOfBirth <= dateBeforeThirtyYears;
        var hourlySalary = (double)value;

        var isValidSalary = isOlderThanThirdyYears ? hourlySalary >= MinSeniorSalary : hourlySalary >= MinJuniorSalary;

        return isValidSalary ? ValidationResult.Success : new ValidationResult(GetErrorMessage());
    }
}

[Route("api/[controller]")]
[ApiController]
public class EmployeeController : ControllerBase
{
    public EmployeeController()
    {
    }

    [HttpPost]
    public Employee Post([FromBody] Employee value)
    {  
            EmployeeValidator validator = new EmployeeValidator();
            var result = validator.Validate(value);

            if (!result.IsValid)
            {
                throw new Exception(result.Errors.ToString());
            }
            return value;
    }
}