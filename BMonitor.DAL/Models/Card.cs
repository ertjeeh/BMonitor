using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BMonitor.DAL.Models;

public class Card
{
    [Key]
    public int Id { get; set; }

    public string Title { get; set; }

    public string Type => this.GetType().Name;

    [JsonIgnore]
    public virtual List<Instance> Instances { get; set; }
}