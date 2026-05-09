using System;
using System.Threading.Tasks.Dataflow;
using Godot;

public partial class Goomba : Enemy
{
	[ExportGroup("External")]
	[Export]
	public CharacterBody2D Character;

	[ExportGroup("Internal")]
	[Export]
	private float Speed;

	[Export]
	private float Scale_Anim;

	[Export]
	private WalkTowardsPlayer WalkTowardsPlayer;

	private int _Direction_Faced = 1;

	public override void _PhysicsProcess(double delta)
	{
		WalkTowardsPlayer.Walk_Towards_Player(Character, Sprite, Speed);
	}

	public void _Begin_walk()
	{
		WalkTowardsPlayer.Is_Active = true;
		Sprite.Play("inactive");
	}
}
