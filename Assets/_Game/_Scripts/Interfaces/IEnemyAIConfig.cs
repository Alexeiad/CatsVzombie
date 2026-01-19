 
public interface IEnemyAIConfig
{
    float PursuitDistance { get; set; }
    float Speed { get; set; }
    float AvoidanceStrength { get; set; }
    float AvoidanceDistance { get; set; }
    float AttackDistance { get; set; }
    float RetreatDistance { get; set; }
    float MinPauseTime { get; set; }
    float MaxPauseTime { get; set; }
    float PatrolVariation { get; set; }
    float TangentialSpeed { get; set; }
    float ChangeDirectionMin { get; set; }
    float ChangeDirectionMax { get; set; }
    float SafeDistance { get; set; }
    float FleeSpeedMultiplier { get; set; }
}