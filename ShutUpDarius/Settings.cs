// ReSharper disable ConvertToConstant.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global
// ReSharper disable InconsistentNaming

namespace ShutUpDarius
{
    public class Settings
    {
        public bool warnAboutTraining = true;
        public bool WarnAboutTraining => warnAboutTraining;

        public int minimumPilotsNeedingTrainingForWarning = 2;
        public int MinimumPilotsNeedingTrainingForWarning => minimumPilotsNeedingTrainingForWarning;

        public int pilotSkillLevelForWhichWarningNotNecessary = 6;
        public int PilotSkillLevelForWhichWarningNotNecessary => pilotSkillLevelForWhichWarningNotNecessary;

        public bool debug = false;
    }
}