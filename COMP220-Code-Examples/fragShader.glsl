#version 330 core

//out vec3 color;
out vec4 color;
in vec3 vertColour;
in vec2 vertUV;

uniform sampler2D texSampler;

void main(){
	color = vec4(1,0,0,1);
	//	color = vertColour;
	color = texture(texSampler, vertUV);//.rgb;
	//color.a = 1;
}
