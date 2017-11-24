Shader "Sprites/RTSFogOfWar" {
	Properties
	{
		_Color("Main Color", Color) = (0.5, 0.5, 0.5, 1)
		_MainTex("Base (RGB), Alpha (A)", 2D) = "white" {}
		_RougTex("Base (RGB), Alpha (A)", 2D) = "white" {}
		_ExploredTex("Explored, Alpha (A)", 2D) = "white" {}
		_NowExploredTex("NowExplored, Alpha (A)", 2D) = "white" {}
		_Alfa("Alfa", Range(0.0, 1.0)) = 1.0
	}

		SubShader
	{
		LOD 200

		Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"
	}

		Pass
	{
		Cull Off
		Lighting Off
		ZWrite Off
		Fog{ Mode Off }
		ColorMask RGB
		Blend SrcAlpha OneMinusSrcAlpha

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag

#include "UnityCG.cginc"

	sampler2D _MainTex;
	sampler2D _RougTex;
	sampler2D _ExploredTex;
	sampler2D _NowExploredTex;
	float4 _MainTex_ST;
	fixed4 _Color;

	float _Alfa;

	uniform int _Points_Length = 0;
	uniform float3 _Points[512];		// (x, y, z) = position
	uniform float1 _Properties[512];	// radius

	struct vertInput
	{
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		float4 pos : POSITION;
	};

	struct vertOutput
	{
		float4 vertex : POSITION;
		float2 texcoord : TEXCOORD0;
		fixed3 worldPos : TEXCOORD1;
	};

	vertOutput vert(vertInput input)
	{
		vertOutput o;
		o.vertex = mul(UNITY_MATRIX_MVP, input.vertex);
		o.texcoord = input.texcoord;
		o.worldPos = mul(_Object2World, input.pos).xyz;
		return o;
	}

	half4 frag(vertOutput input) : COLOR
	{
		float T = _Time*0.1f;
		fixed4 col = tex2D(_MainTex, input.texcoord - T);
		fixed4 rip = tex2D(_RougTex, input.texcoord - T);
		fixed4 nowCol = tex2D(_NowExploredTex, input.texcoord);
		fixed4 exploredCol = tex2D(_ExploredTex, input.texcoord);
		float exploredAlfa = exploredCol.a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(0.025, 0.025)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(0.025, -0.025)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(-0.025, -0.025)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(0.025, 0.025)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(0.05, 0.05)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(0.05, -0.05)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(-0.05, -0.05)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(0.05, 0.05)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(0, 0.025)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(0, -0.025)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(-0.025, 0)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(0.025, 0)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(05, 0.05)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(0, -0.05)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(-0.05, 0)).a;
		exploredAlfa += tex2D(_ExploredTex, input.texcoord + half2(0.05, 0)).a;
		exploredAlfa += tex2D(_NowExploredTex, input.texcoord + half2(0.025, 0)).a;
		exploredAlfa += tex2D(_NowExploredTex, input.texcoord + half2(0.025, -0.025)).a;
		exploredAlfa += tex2D(_NowExploredTex, input.texcoord + half2(0.025, 0.025)).a;
		exploredAlfa += tex2D(_NowExploredTex, input.texcoord + half2(-0.025, 0)).a;
		exploredAlfa += tex2D(_NowExploredTex, input.texcoord + half2(-0.025, -0.025)).a;
		exploredAlfa += tex2D(_NowExploredTex, input.texcoord + half2(-0.025, 0.025)).a;
		exploredAlfa += tex2D(_NowExploredTex, input.texcoord + half2(0, 0)).a;
		exploredAlfa += tex2D(_NowExploredTex, input.texcoord + half2(0, -0.025)).a;
		exploredAlfa += tex2D(_NowExploredTex, input.texcoord + half2(0, 0.025)).a;

		exploredAlfa = saturate(exploredAlfa / 26 * 1.3);
		half h = 0;
		for (int i = 0; i < _Points_Length; i++)
		{
			float _R = _Properties[i];
			float _Transparency = saturate(0.9*rip.rgb);
			half dist = distance(input.worldPos.xz, _Points[i].xz);
			half hi = 1 - (dist / _R);
			if (dist < _R)
			{
				hi = saturate(hi / (1 - _Transparency + nowCol.a));
				// + nowCol.a обеспечивает туман когда смотришь снизу вверх 
			}
			h = max(h, saturate(hi * 2));
		}
		h = saturate(h);
		half4 color2 = half4(col.r*_Color.r, col.g*_Color.g, col.b*_Color.b, _Alfa * min(exploredAlfa, saturate(col.a*(1 - h))));
		return color2;
	}
		ENDCG
	}
	}
}
