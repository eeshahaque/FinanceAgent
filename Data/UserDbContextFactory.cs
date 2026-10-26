using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FinanceAgent.Data;

// Creates a FinanceContext scoped to the signed-in user. Services and pages use this instead of
// IDbContextFactory directly, so every query is automatically limited to that user's rows.
public class UserDbContextFactory
{
    private readonly IDbContextFactory<FinanceContext> _inner;
    private readonly AuthenticationStateProvider _auth;

    public UserDbContextFactory(IDbContextFactory<FinanceContext> inner, AuthenticationStateProvider auth)
    {
        _inner = inner;
        _auth = auth;
    }

    public async Task<FinanceContext> CreateDbContextAsync()
    {
        var state = await _auth.GetAuthenticationStateAsync();
        var userId = state.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("You need to be signed in.");

        var db = await _inner.CreateDbContextAsync();
        db.CurrentUserId = userId;
        return db;
    }
}
