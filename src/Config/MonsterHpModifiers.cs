namespace SlaySpoon.Config;

/// <summary>Enemy max HP multiplier. One empty sealed subclass per percent, named <Family><Percent>; the value travels as the modifier id.</summary>
public abstract class SpoonMonsterHp : SpoonPercentModifier
{
	protected override string FamilyKey => "SPOON_MONSTER_HP";
}

public sealed class SpoonMonsterHp25 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp50 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp75 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp100 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp125 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp150 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp175 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp200 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp225 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp250 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp275 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp300 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp325 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp350 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp375 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp400 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp425 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp450 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp475 : SpoonMonsterHp { }
public sealed class SpoonMonsterHp500 : SpoonMonsterHp { }
