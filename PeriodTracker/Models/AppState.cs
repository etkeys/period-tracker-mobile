
using System.ComponentModel.DataAnnotations;

namespace PeriodTracker;

public class AppState
{
    [Key]
    public AppStateProperty AppStatePropertyId {get; set;}

    [MaxLength(2000)]
    public string Value {get; set;} = string.Empty;

}