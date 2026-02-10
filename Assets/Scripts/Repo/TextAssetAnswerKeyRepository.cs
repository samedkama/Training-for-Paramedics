using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using VR.Triage.Models;
using VR.Triage.Repo;


namespace VR.Triage.Repo
{
    // Loads AnswerKey JSON data from the Resources/AnswerKeys folder.
    public interface IAnswerKeyRepository
    {
        Task<AnswerKeyDefinition> LoadAnswerKeyAsync(string scenarioId);
    }

    public class TextAssetAnswerKeyRepository : IAnswerKeyRepository
    {
        public async Task<AnswerKeyDefinition> LoadAnswerKeyAsync(string scenarioId)
        {
            // Resources.Load expects a path relative to Assets/Resources and without extension.
            var ta = Resources.Load<TextAsset>($"AnswerKeys/{scenarioId}");

            if (ta == null)
                throw new System.Exception($"AnswerKey not found: Resources/AnswerKeys/{scenarioId}.json");

            await Task.Yield(); // Keeps async API shape consistent with other repositories.
            return JsonConvert.DeserializeObject<AnswerKeyDefinition>(ta.text);
        }
    }
}
