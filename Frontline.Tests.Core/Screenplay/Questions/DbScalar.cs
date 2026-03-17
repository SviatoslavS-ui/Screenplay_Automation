using Frontline.Tests.Core.Screenplay.Abilities;
using Frontline.Tests.Core.Screenplay.Core;
using Microsoft.Data.SqlClient;

namespace Frontline.Tests.Core.Screenplay.Questions;

/// <summary>Executes a parameterised scalar SQL query and returns the typed result.</summary>
public class DbScalar<T>(string sql, params SqlParameter[] parameters) : IQuestion<T?>
{
    public async Task<T?> AnswerAsync(Actor actor)
    {
        var db = actor.UsesAbility<DatabaseAbility>();
        return await db.ScalarAsync<T>(sql, parameters);
    }
}
