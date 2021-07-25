﻿#version 410 core

layout(location = 0) in vec3 aPosition;

//in vec3 aPosition;


out vec2 texCoord;


void main(void)
{
	gl_Position = vec4(aPosition.x, aPosition.y, aPosition.z, 1);

	texCoord = aPosition.xy;
//	texCoord = vec2(0,0);
}


