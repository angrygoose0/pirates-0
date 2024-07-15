Shader "Custom/GooShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _TimeScale ("Time Scale", Float) = 1.0
        _Speed ("Speed", Float) = 1.0
        _Distortion ("Distortion", Float) = 0.1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _TimeScale;
            float _Speed;
            float _Distortion;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float time = _Time.y * _TimeScale;
                float2 uvDistorted = i.uv + _Distortion * sin(i.uv.y * 10.0 + time * _Speed);
                fixed4 col = tex2D(_MainTex, uvDistorted) * _Color;
                return col;
            }
            ENDCG
        }
    }
}
