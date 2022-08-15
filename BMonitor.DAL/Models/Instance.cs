using System.ComponentModel.DataAnnotations;

namespace BMonitor.DAL.Models;

public class Instance
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }
    public string Description { get; set; }

    public virtual List<Card> Cards { get; set; }
}