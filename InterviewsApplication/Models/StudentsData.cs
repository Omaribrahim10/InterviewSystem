using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using InterviewsApplication.Models.Enums;

namespace InterviewsApplication.Models
{
    public class StudentsData
    {
        [Key]
        public string UniversityID { get; set; }
        public string ReferralSource { get; set; }
        public string? Activities { get; set; }
        public string? Awards { get; set; }
        public DateTime SubmittedAt { get; set; }  
        public string ImageAttach { get; set; }
        public MailResultEnum? MailResult { get; set; }
        public int? MailID { get; set; }

        [ForeignKey(nameof(UniversityID))]
        public TestStudent TestStudent { get; set; }

        [ForeignKey(nameof(MailID))]
        public MailingContent MailingContent { get; set; }
    }
}
