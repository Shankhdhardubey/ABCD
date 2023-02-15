using System.ComponentModel;

namespace DataModelMigration.Common
{
    public enum MigrationTypes : byte
    {
        [Description("InitialMigration")]
        Initialize = 0,

        [Description("CaptionMigration")]
        Caption,

        [Description("GenericLookUpMigration")]
        GenericLookUp,

        [Description("CustomLookUpMigration")]
        CustomLookUp,

        [Description("GeneralPolicyProductMigration")]
        GeneralPolicyProduct,

        [Description("ProductMigration")]
        Product,

        [Description("ConfigurationMigration")]
        Configuration,

        [Description("UserMigration")]
        User,

        [Description("NonClientPartyMigration")]
        NonClientParty,

        [Description("PartyMigration")]
        Party,

        [Description("PartyExtraMigration")]
        PartyExtra,

        [Description("PolicyMigration")]
        Policy,

        [Description("RiskMigration")]
        Risk,

        [Description("GeneralPolicyDataMigration")]
        GeneralPolicyData,

        [Description("ProductRiskDataMigration")]
        ProductRiskData,

        [Description("RenewalMigration")]
        Renewal,

        [Description("TransactionMigration")]
        Transaction,

        [Description("DiaryTaskMigration")]
        DiaryTask,

        [Description("EventMigration")]
        Event,

        [Description("FinalMigration")]
        Final
    }
}
