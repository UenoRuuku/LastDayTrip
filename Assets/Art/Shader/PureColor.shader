// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/PureColor"
{
    Properties
    {
        _MainTex("Main Tex",2D) = "white"{}
    }
        SubShader
    {
        Tags{ "RenderType" = "Transparent" "Queue"="Overlay"}
        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Tint;
            
            struct a2v
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                fixed4 color : COLOR;
            };
            struct v2f
            {
                float2 uv :TEXCOORD0;
                float4 pos : SV_POSITION;
                fixed4 color : COLOR;
            };
            v2f vert(a2v a)
            {
                v2f f;
                f.uv = TRANSFORM_TEX(a.texcoord, _MainTex);
                f.pos = UnityObjectToClipPos(a.vertex);
                f.color = a.color;
                return f;
            }
            fixed4 frag (v2f i) :SV_Target
            {
                fixed4 col = tex2D(_MainTex,i.uv);            
                fixed4 colo = col*fixed4(0, 0, 0, i.color.a) + fixed4(i.color.rgb,0);
                return colo;
            }
            ENDCG
        }
    }
}
