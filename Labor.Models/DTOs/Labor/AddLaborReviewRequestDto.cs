using System.ComponentModel.DataAnnotations;

namespace Labor.Models.DTOs.Labor;

public class AddLaborReviewRequestDto
{
    [Required]
    public int OrderItemId { get; set; }

    [Required]
    public int LaborId { get; set; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }
}

public class UpdateLaborReviewRequestDto
{
    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [StringLength(1000)]
    public string? Comment { get; set; }
}

public class AddLaborReviewResponseDto
{
    public int ReviewId { get; set; }
}
