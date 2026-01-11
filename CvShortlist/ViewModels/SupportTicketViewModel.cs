using System.ComponentModel.DataAnnotations;
using CvShortlist.Models;

namespace CvShortlist.ViewModels
{
    public class SupportTicketViewModel
    {
        private const int MessageMaxLength = 5000;

        [Required(ErrorMessage = "Name is required")]
        [MaxLength(SupportTicket.NameMaxLength, ErrorMessage = "The entered name is too long.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [MaxLength(SupportTicket.EmailMaxLength, ErrorMessage = "The entered email is too long.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Message is required")]
        [MinLength(10, ErrorMessage = "Minimum message length is 10 characters.")]
        [MaxLength(MessageMaxLength, ErrorMessage = "The entered message is too long.")]
        public string Message { get; set; } = string.Empty;

        [Required(ErrorMessage = "Captcha is required")]
        public string CaptchaInput { get; set; } = string.Empty;
    }
}
