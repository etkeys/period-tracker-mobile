
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace PeriodTracker;

[EntityTypeConfiguration(typeof(AppStateConfiguration))]
public class AppState
{
    [Key]
    public AppStateProperty AppStatePropertyId {get; set;}

    [MaxLength(2000)]
    [Unicode(false)]
    public string Value {get; set;} = string.Empty;

}