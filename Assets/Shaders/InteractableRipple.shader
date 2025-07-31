Shader "Custom/RippleOverlay"
{
    Properties
    {
        _RippleColor ("Ripple Color", Color) = (0.2, 0.6, 1, 0.5)
        _RippleSpeed ("Ripple Speed", Float) = 3.0
        _RippleSize ("Ripple Size", Float) = 8.0
        _RippleIntensity ("Ripple Intensity", Float) = 0.4
        _RippleCenter ("Ripple Center", Vector) = (0.5, 0.5, 0, 0)
        _RippleRadius ("Ripple Radius", Float) = 1.0
    }
    
    SubShader
    {
        Tags {"Queue"="Transparent+1" "RenderType"="Transparent"}
        
        // Render AFTER the base object
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        
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

            fixed4 _RippleColor;
            float _RippleSpeed;
            float _RippleSize;
            float _RippleIntensity;
            float4 _RippleCenter;
            float _RippleRadius;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate distance from ripple center
                float2 rippleUV = i.uv - _RippleCenter.xy;
                float dist = length(rippleUV);
                
                // Create multiple ripple waves
                float time = _Time.y * _RippleSpeed;
                float ripple1 = sin((dist * _RippleSize - time) * 6.28318);
                float ripple2 = sin((dist * _RippleSize - time + 1.0) * 6.28318) * 0.5;
                float ripple3 = sin((dist * _RippleSize - time + 2.0) * 6.28318) * 0.25;
                
                float totalRipple = (ripple1 + ripple2 + ripple3) * _RippleIntensity;
                
                            
                // Use ripple effect WITHOUT distance fade
                float rippleEffect = totalRipple;
                
                // Only show the ripple effect (transparent elsewhere)
                float alpha = abs(rippleEffect) * _RippleColor.a;
                
                return fixed4(_RippleColor.rgb, alpha);
            }
            ENDCG
        }
    }
}
