using System.ComponentModel.DataAnnotations;

namespace BMonitor.DAL.Models;

public class Setting
{
    [Key]
    public int Id { get; set; }

    [MaxLength(128)]
    public string Key { get; set; }

    [MaxLength(1024)]
    public string Value { get; set; }
}