using Frontline.Tests.Core.Screenplay.Abilities;
using Frontline.Tests.Core.Screenplay.Core;
using Microsoft.Data.SqlClient;

namespace Frontline.Tests.Core.Screenplay.Interactions;

/// <summary>Executes a parameterised non-query (INSERT, UPDATE, DELETE) against the database. Returns rows affected.</summary>
public class DbExecute(string sql, params SqlParameter[] parameters) : IInteraction
{
    public int RowsAffected { get; private set; }

    public string Description => "Execute database command";

    public async Task PerformAsync(Actor actor)
    {
        var db = actor.UsesAbility<DatabaseAbility>();
        RowsAffected = await db.ExecuteAsync(sql, parameters);
    }
}
