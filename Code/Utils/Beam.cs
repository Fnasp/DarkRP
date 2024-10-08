namespace Dxura.Darkrp;

public sealed class Beam : Component
{
	VectorLineRenderer LineRenderer1;
	VectorLineRenderer LineRenderer2;

	LegacyParticleSystem particleSystem;

	[Property] public string? StartParticle { get; set; }
	[Property] public bool EffectStart { get; set; } = true;
	[Property] public bool RunBySelf { get; set; }
	[Property] public float Scale { get; set; } = 1f;
	[Property] public float Noise { get; set; } = 1f;
	[Property] public Curve EffectCurve1 { get; set; }
	[Property] public Curve EffectCurve2 { get; set; }
	[Property] public bool enabled { get; set; }
	[Property] public Vector3 Base { get; set; }
	[Property] public float pointDistance { get; set; } = 10;
	[Property] public GameObject ObjectStart { get; set; }
	[Property] public GameObject ObjectEnd { get; set; }
	protected override void OnStart()
	{
		LineRenderer2 = Components.Create<VectorLineRenderer>();
		LineRenderer2.Points = new List<Vector3> { Vector3.Zero, Vector3.Zero };
		LineRenderer2.Color = Color.Cyan;
		LineRenderer2.Width = EffectCurve1;
		LineRenderer2.RunBySelf = false;
		LineRenderer2.Noise = Noise;
		LineRenderer2.Opaque = false;

		LineRenderer1 = Components.Create<VectorLineRenderer>();
		LineRenderer1.Points = new List<Vector3> { Vector3.Zero, Vector3.Zero };
		LineRenderer1.Color = Color.Cyan;
		LineRenderer1.Width = EffectCurve2;
		LineRenderer1.RunBySelf = false;
		LineRenderer1.Noise = Noise * 0.2f;
		LineRenderer1.Opaque = false;

		if ( EffectStart && StartParticle != null ) particleSystem = CreateParticleSystem( StartParticle );


	}
	protected override void OnPreRender()
	{
		if ( !LineRenderer1.IsValid() || !LineRenderer2.IsValid() )
			return;

		if ( RunBySelf )
		{
			CreateEffect( ObjectStart.Transform.Position, ObjectEnd.Transform.Position, ObjectStart.Transform.World.Forward );
			LineRenderer1.Run();
			LineRenderer2.Run();
		}

		LineRenderer1.Enabled = enabled;
		LineRenderer2.Enabled = enabled;
		if ( EffectStart ) particleSystem.Enabled = enabled;
		if ( EffectStart ) particleSystem.Transform.Position = Base;
		if ( !enabled && !RunBySelf ) return;
		LineRenderer1.Run();
		LineRenderer2.Run();
	}
	protected override void OnFixedUpdate()
	{

	}

	public void CreateEffect( Vector3 start, Vector3 end, Vector3 dir )
	{
		LineRenderer1.Points = GetCurvedPoints( start, dir, end, (int)(MathF.Round( Vector3.DistanceBetween( start, end ) ) / pointDistance) );
		LineRenderer2.Points = GetCurvedPoints( start, dir, end, (int)(MathF.Round( Vector3.DistanceBetween( start, end ) ) / pointDistance * 2) );


	}

	private LegacyParticleSystem CreateParticleSystem( string particle )
	{
		var gameObject = Scene.CreateObject();
		gameObject.SetParent( GameObject );
		gameObject.Transform.LocalPosition = Vector3.Zero;
		gameObject.Transform.LocalRotation = Angles.Zero;
		gameObject.Transform.LocalScale = Vector3.One * 0.1f;


		var p = gameObject.Components.Create<LegacyParticleSystem>();
		p.Particles = ParticleSystem.Load( particle );
		gameObject.Transform.ClearInterpolation();


		return p;
	}



	public static List<Vector3> GetCurvedPoints( Vector3 start, Vector3 initialDirection, Vector3 end, int numberOfPoints )
	{
		var points = new List<Vector3>();

		// Calculate the control points
		var control1 = start + initialDirection * 10;
		var control2 = end - (end - start).Normal * 10 * initialDirection.Length;

		var step = 1.0f / (numberOfPoints - 1);

		for ( var i = 0; i < numberOfPoints; i++ )
		{
			var t = i * step;
			var point = CalculateCubicBezierPoint( t, start, control1, control2, end );
			points.Add( point );
		}

		return points;
	}

	private static Vector3 CalculateCubicBezierPoint( float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3 )
	{
		var u = 1 - t;
		var tt = t * t;
		var uu = u * u;
		var ttt = tt * t;
		var uuu = uu * u;

		var p = uuu * p0;
		p += 3 * uu * t * p1;
		p += 3 * u * tt * p2;
		p += ttt * p3;

		return p;
	}
}
