﻿using System.Text.Json;
using System.Text.Json.Serialization;
using Prospect.Server.Api.Models.Client.Data;
using Prospect.Server.Api.Services.Database;

namespace Prospect.Server.Api.Services.UserData;

public class UserTechTreeNodeData {
    [JsonPropertyName("nodeInProgress")]
    public string NodeInProgress { get; set; }
    [JsonPropertyName("totalUpgrades")]
    public int TotalUpgrades { get; set; }
    [JsonPropertyName("nodes")]
    public Dictionary<string, FYTechTreeNodeStatus> Nodes { get; set; }
}

public class CharacterTechTreeBonuses {
    [JsonPropertyName("aurumCap")]
    public int AurumCap { get; set; }
    [JsonPropertyName("aurumRate")]
    public float AurumRate { get; set; }
    [JsonPropertyName("kmarksCap")]
    public int KmarksCap { get; set; }
    [JsonPropertyName("kmarksRate")]
    public int KmarksRate { get; set; }
    [JsonPropertyName("crateTier")]
    public int CrateTier { get; set; }
    [JsonPropertyName("addStashSize")]
    public int AddStashSize { get; set; }
    [JsonPropertyName("addSafePocketSize")]
    public int AddSafePocketSize { get; set; }
    [JsonPropertyName("upgradeSpeedFactor")]
    public float UpgradeSpeedFactor { get; set; }
}

public class UserDataService
{
    private readonly ILogger<UserDataService> _logger;
    private readonly DbUserDataService _dbUserDataService;
    private readonly TitleDataService _titleDataService;

    public UserDataService(ILogger<UserDataService> logger, DbUserDataService dbUserDataService, TitleDataService titleDataService)
    {
        _logger = logger;
        _dbUserDataService = dbUserDataService;
        _titleDataService = titleDataService;
    }

    /// <summary>
    ///     Initialize data for the given PlayFabId.
    /// </summary>
    public async Task InitAsync(string playFabId)
    {
        // TODO: Do not call this on each InitAsync
        var titleData = _titleDataService.Find(new List<string>{"TechTreeNodes", "Blueprints"});
        var techTreeNodes = JsonSerializer.Deserialize<Dictionary<string, TitleDataTechTreeNodeInfo>>(titleData["TechTreeNodes"]);

        UserTechTreeNodeData userTechTreeNodesData = new UserTechTreeNodeData {
            NodeInProgress = "",
            TotalUpgrades = 0,
            Nodes = [],
        };
        foreach (var nodeData in techTreeNodes) {
            userTechTreeNodesData.Nodes[nodeData.Key] = new FYTechTreeNodeStatus {
                NodeID = nodeData.Key,
                Level = 0,
                UpgradeStartedTime = new CloudScript.Models.Data.FYTimestamp {
                    Seconds = 0,
                }
            };
        }

        var blueprints = JsonSerializer.Deserialize<Dictionary<string, TitleDataBlueprintInfo>>(titleData["Blueprints"]);

        // TODO: For testing purposes
        List<FYCustomItemInfo> inventory = new List<FYCustomItemInfo>();
        foreach (var dict in blueprints)
        {
            var blueprint = dict.Value;
            if (blueprint.Kind != "Material")
            {
                continue;
            }
            for (int i = 0; i < 3; i++)
            {
                inventory.Add(new FYCustomItemInfo {
                    Amount = 10000,
                    BaseItemId = blueprint.Name,
                    Durability = blueprint.DurabilityMax,
                    ItemId = Guid.NewGuid().ToString(),
                    Insurance = "",
                    InsuranceOwnerPlayfabId = "",
                    ModData = new FYModItems { M = [] },
                    Origin = new FYItemOriginBackend { G = "", P = "", T = "" },
                    InsuredAttachmentId = "",
                    PrimaryVanityId = 0,
                    SecondaryVanityId = 0,
                    RolledPerks = [],
                });
            }
        }

        // TODO: Proper objects.
        var defaultData = new Dictionary<string, (bool isPublic, string value)>
        {
            // GET /Client/GetUserData & GET /Client/GetUserReadOnlyData
            // TODO: Contracts and factions progression
            ["FactionProgressionKorolev"] = (false, "0"),
            ["FactionProgressionICA"] = (false, "0"),
            ["FactionProgressionOsiris"] = (false, "0"),
            ["ContractsActive"] = (false, $"{{\"userId\":\"{playFabId}\",\"error\":\"\",\"contracts\":[]}}"),
            ["ContractsOneTimeCompleted"] = (false, "{\"contractsIds\":[]}"),
            ["OnboardingProgression"] = (false, $"{{\"userId\":\"{playFabId}\",\"currentMissionID\":\"TalkToBadum\",\"progress\":0,\"showPopup\":true}}"),

            // TODO: Notifications
            ["Notifications"] = (false, "{\"notifications\":[]}"), // FYNotificationDescription?
            ["Notifications_Account"] = (false, "{\"notifications\":[]}"), // FYNotificationDescription?

            // TODO: Fortuna Pass Season 2
            ["FortunaPass2_DailyCompletedActions"] = (false, "{\"actions\":{},\"lastDailyCapResetTimeUtc\":{\"seconds\":0}}"),
            ["FortunaPass2_ClaimedRewards"] = (false, "{\"rewardsIds\":[]}"),
            ["FortunaPass2_PremiumUnlock"] = (false, "false"),
            ["FortunaPass2_SeasonXp"] = (false, "0"),

            // TODO: Fortuna Pass Season 3
            ["FortunaPass3_DailyCompletedActions"] = (false, "{\"actions\":{},\"lastDailyCapResetTimeUtc\":{\"seconds\":0}}"),
            ["FortunaPass3_ClaimedRewards"] = (false, "{\"rewardsIds\":[]}"),
            ["FortunaPass3_PremiumUnlock"] = (false, "false"),
            ["FortunaPass3_SeasonXp"] = (false, "0"),

            // TODO: Quarters
            ["TechTreeNodeData"] = (false, JsonSerializer.Serialize(userTechTreeNodesData)),
            ["CraftingTimer__2022_05_12"] = (false, "{\"itemId\":\"\",\"utcTimestampWhenCraftingStarted\":{\"seconds\":0}}"),
            ["Generators__2021_09_09"] = (false, "[{\"generatorId\":\"playerquarters_gen_aurum\",\"lastClaimTime\":{\"seconds\":0}},{\"generatorId\":\"playerquarters_gen_kmarks\",\"lastClaimTime\":{\"seconds\":0}},{\"generatorId\":\"playerquarters_gen_crate\",\"lastClaimTime\":{\"seconds\":0}}]"),
            ["ClaimableStarterPacks"] = (false, "{\"packages\":[]}"), // FYClaimableStarterPackPackageResult?
            ["ClaimableVictimCompensations"] = (false, "{\"packages\":[]}"), // FYBackendClaimableVictimCompensationWrapper?
            ["PlayerQuartersLevel"] = (false, "{\"level\":1,\"upgradeStartedTime\":{\"seconds\":0}}"),
            ["TwitchDropRewards"] = (false, "{\"grantedItems\":[]}"), // FYInventoryItem
            ["InsuranceInvoice"] = (false, "{}"),
            ["InsuranceClaims"] = (false, "{\"packages\":[]}"), // FYBackendInsurancePayoutPackage. Limited to 20.

            // TODO: Player data
            ["LOADOUT"] = (false, $"{{\"shield\":\"\",\"helmet\":\"\",\"weaponOne\":\"\",\"weaponTwo\":\"\",\"bag\":\"\",\"bagItemsAsJsonStr\":\"\",\"safeItemsAsJsonStr\":\"\"}}"),
            ["SessionData"] = (false, "{}"),
            ["SeasonWipe"] = (false, "{}"),
            ["VeteranPoints"] = (false, "{}"),
            ["FreeLoadout"] = (false, "{}"), // Season 3

            // Function-specific data
            // Used to optimize bonuses calculation and further validations without getting the user data and computing bonuses in runtime.
            ["CharacterTechTreeBonuses"] = (false, "{\"aurumCap\":0,\"aurumRate\":0,\"kmarksCap\":0,\"kmarksRate\":0,\"crateTier\":0,\"stashSize\":0,\"safePocketSize\":0,\"upgradeSpeed\":1.0}"),
            ["GlobalVanity"] = (false, $"{{\"activeGlobalVanityIds\":[\"Season03_Spray_13\",\"Banner_MIne\",\"Emote_Chill_01\",\"\",\"\",\"\",\"\",\"\"], \"droppodId\": \"VDP_Omega02\"}}"),
            // NOTE: Default CharacterVanity is based on Season 2 IDs.
            ["CharacterVanity"] = (false, $"{{\"userId\":\"{playFabId}\",\"head_item\":{{\"id\":\"E01_G02_Head01\",\"materialIndex\":0}},\"boots_item\":{{\"id\":\"StarterOutfit01_Boots_M\",\"materialIndex\":0}},\"chest_item\":{{\"id\":\"StarterOutfit01_Chest_M\",\"materialIndex\":0}},\"glove_item\":{{\"id\":\"StarterOutfit01_Gloves_M\",\"materialIndex\":0}},\"base_suit_item\":{{\"id\":\"StarterOutfit01M_BaseSuit\",\"materialIndex\":0}},\"melee_weapon_item\":{{\"id\":\"Melee_Omega\",\"materialIndex\":0}},\"body_type\":1,\"archetype_id\":\"E01_G02\",\"slot_index\":0}}"),
            // ["Inventory"] = (false, "[]"),
            ["Inventory"] = (false, JsonSerializer.Serialize(inventory)),
            ["VanityItems"] = (false, "[]"),
            ["RetentionBonus"] = (false, "{\"claimedAll\":false,\"daysClaimed\":0,\"lastClaimTime\":{\"seconds\":0}}"),
            ["Balance"] = (false, "{\"IN\": 0,\"AU\": 800,\"SC\": 30000}"),
        };

        foreach (var (key, (isPublic, value)) in defaultData)
        {
            if (!await _dbUserDataService.HasAsync(playFabId, key))
            {
                await _dbUserDataService.InsertAsync(playFabId, key, value, isPublic);
            }
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="currentUserId">
    ///     The authenticated user id.
    /// </param>
    /// <param name="requestUserId">
    ///     The requested user id.
    /// </param>
    /// <param name="keys"></param>
    /// <returns></returns>
    public async Task<Dictionary<string, FUserDataRecord>> FindAsync(
        string currentUserId,
        string requestUserId,
        List<string> keys)
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            throw new ArgumentNullException(nameof(currentUserId));
        }

        if (requestUserId == null)
        {
            requestUserId = currentUserId;
        }

        var other = currentUserId != requestUserId;
        if (other) {
            Console.WriteLine("aboba");
        }
        var result = new Dictionary<string, FUserDataRecord>();

        if (keys != null && keys.Count > 0)
        {
            foreach (var key in keys)
            {
                var data = await _dbUserDataService.FindAsync(requestUserId, key);
                if (data == null)
                {
                    // TODO: Error?
                    continue;
                }

                if (!data.Public && other)
                {
                    // TODO: Error?
                    continue;
                }

                result.Add(data.Key, new FUserDataRecord
                {
                    LastUpdated = data.LastUpdated,
                    Permission = data.Public ? UserDataPermission.Public : UserDataPermission.Private,
                    Value = data.Value
                });
            }
        }
        else
        {
            // TODO: Getting ALL user data seems to be required only for fetching the player's passive generators data.
            // This is not OK and normally should not be allowed.
            var cursor = await _dbUserDataService.AllAsync(requestUserId, other);

            while (await cursor.MoveNextAsync())
            {
                foreach (var data in cursor.Current)
                {
                    result.Add(data.Key, new FUserDataRecord
                    {
                        LastUpdated = data.LastUpdated,
                        Permission = data.Public ? UserDataPermission.Public : UserDataPermission.Private,
                        Value = data.Value
                    });
                }
            }
        }

        return result;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="currentUserId">
    ///     The authenticated user id.
    /// </param>
    /// <param name="targetUserId">
    ///     The target user id.
    /// </param>
    /// <param name="changes"></param>
    /// <returns></returns>
    public async Task UpdateAsync(
        string currentUserId,
        string? targetUserId,
        Dictionary<string, string> changes)
    {
        if (changes.Count == 0)
        {
            return;
        }

        if (string.IsNullOrEmpty(currentUserId))
        {
            throw new ArgumentNullException(nameof(currentUserId));
        }

        if (targetUserId == null)
        {
            targetUserId = currentUserId;
        }

        // Whether we are updating someone else.
        var other = currentUserId != targetUserId;

        foreach (var (key, value) in changes)
        {
            var data = await _dbUserDataService.FindAsync(currentUserId, key);
            if (data == null)
            {
                if (other)
                {
                    _logger.LogWarning("User {PlayFabId} attempted to update non-existing key {Key} of another user {PlayFabIdOther}", currentUserId, key, targetUserId);
                }
                else
                {
                    _logger.LogDebug("User {PlayFabId} created key {Key}", currentUserId, key);
                    await _dbUserDataService.InsertAsync(currentUserId, key, value, false);
                }

                continue;
            }

            if (!data.Public && other)
            {
                _logger.LogWarning("User {PlayFabId} attempted to update non-public key {Key} of another user {PlayFabIdOther}", currentUserId, key, data.PlayFabId);
                continue;
            }

            await _dbUserDataService.UpdateValueAsync(data.Id, value);
        }
    }
}