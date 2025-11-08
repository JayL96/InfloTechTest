using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserManagement.Models;

public class User
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }

    [Required(ErrorMessage = "Forename is required")]
    [StringLength(50, ErrorMessage = "Forename must be under 50 characters")]
    public string Forename { get; set; } = default!;

    [Required(ErrorMessage = "Surname is required")]
    [StringLength(50, ErrorMessage = "Surname must be under 50 characters")]
    public string Surname { get; set; } = default!;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string Email { get; set; } = default!;

    [Required(ErrorMessage = "Date of birth is required")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy}", ApplyFormatInEditMode = true)]
    public DateTime DateOfBirth { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }
}
