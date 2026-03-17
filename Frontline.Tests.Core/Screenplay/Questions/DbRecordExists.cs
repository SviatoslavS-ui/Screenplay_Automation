using Frontline.Tests.Core.Screenplay.Abilities;
using Frontline.Tests.Core.Screenplay.Core;
using Microsoft.Data.SqlClient;

namespace Frontline.Tests.Core.Screenplay.Questions;

/// <summary>Checks whether at least one record matches the given SQL query. Returns true/false.</summary>
public class DbRecordExists(string sql, params SqlParameter[] parameters) : IQuestion<bool>
{
    public async Task<bool> AnswerAsync(Actor actor)
    {
        var db = actor.UsesAbility<DatabaseAbility>();
        var count = await db.ScalarAsync<int>(sql, parameters);
        return count > 0;
    }
}
