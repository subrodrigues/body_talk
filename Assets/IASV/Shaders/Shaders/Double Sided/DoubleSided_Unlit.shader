// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/DoubleSided/Unlit" {
Properties {

}
SubShader {
Cull off   
        Pass {
ColorMaterial AmbientAndDiffuse
            }

}
}
