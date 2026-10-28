namespace SlaySpoon.Config;

/// <summary>Enemy damage multiplier. One empty sealed subclass per percent, named <Family><Percent>; the value travels as the modifier id.</summary>
public abstract class SpoonMonsterDamage : SpoonPercentModifier
{
	protected override string FamilyKey => "SPOON_MONSTER_DAMAGE";
}

public sealed class SpoonMonsterDamage25 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage50 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage75 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage100 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage125 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage150 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage175 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage200 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage225 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage250 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage275 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage300 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage325 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage350 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage375 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage400 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage425 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage450 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage475 : SpoonMonsterDamage { }
public sealed class SpoonMonsterDamage500 : SpoonMonsterDamage { }
