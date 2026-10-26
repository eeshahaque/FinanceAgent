namespace FinanceAgent.Models;

// Rows that belong to one user. FinanceContext filters these to the signed-in user
// and stamps UserId on new rows automatically.
public interface IUserOwned
{
    string? UserId { get; set; }
}
