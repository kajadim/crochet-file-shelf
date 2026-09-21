using System.ComponentModel.DataAnnotations;

namespace backend.Dtos.Folders
{
    public class CreateFolderRequest
    {
        [Required]
        [MaxLength(150)]
        [RegularExpression(@"(?s)^\s*\S.*$", ErrorMessage = "Name cannot be blank.")]
        public string Name { get; set; } = null!;

        public Guid? ParentFolderId { get; set; }
    }
}
