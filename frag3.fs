#version 330 core
out vec4 FragColor;
varying vec2 position;
uniform vec3 ourColor;
uniform float time;
uniform float c = 0.1;

uniform vec3 bh = vec3(0., 0., 0);
uniform vec3 player = vec3(0., 0., 4.);
uniform float rs = 0.01;

uniform bool bh_active = false;

vec4 colorAt(vec3 pos, float t) {
	float d;
	d =  time - t;
	d = atan(pos.y, pos.x);
	if (length(pos - bh) <= rs && bh_active) {
		float f = 0; //1 - length(pos - bh) / rs;
		f = sqrt(f);
		// f = 0;
		return vec4(f,f,f,1);
	}
	else {
		// d = 3*pos.x + 2*time;
		vec3 n = 0.5*normalize(pos)+0.5;
		return vec4(n.x, n.y, n.z, 1.);
		// d = atan((player-pos).y, (player-pos).x);
		// return vec4(0.5*sin(d+2) + 0.5, 0.5*sin(d+4) + 0.5, 0.5*sin(d) + 0.5, 1.0f);
		// return vec4(0.5*sin(d+2) + 0.5, 0.5*sin(d+4) + 0.5, 0.5*sin(d) + 0.5, 1.0f);
		// return vec4(0.5*pos.x + 0.5, 0.5*pos.y + 0.5, 1.0f, 1.0f);
		// return vec4(0.5*(1+sin(d)), 0.5*(1-sin(d)), 0., 1.);
	}
}

vec3 rotate(vec3 x, vec3 u, float t) {
	mat3 W;
	W[0] = vec3(0, -u.z, u.y);
	W[1] = vec3(u.z, 0, -u.x);
	W[2] = vec3(-u.y, u.x, 0);
	mat3 R = mat3(1.0) + (sin(t) * W) + (2*sin(t/2)*sin(t/2) * W  * W);
	return R*x;
}

float w(float r) {
	return 1-rs/r;
}

float wp(float r) {
	return rs/(r*r);
}

float f(float r, float J) {
	// return ( w(r)*wp(r)*( 1 - J*J*w(r)/(r*r)) + 0.5*J*J*w(r)*w(r)* (2*w(r)/(r*r*r) - wp(r)/(r*r) )  );
	return w(r)*wp(r) * ( 1 - 3/2*w(r)*J*J/(r*r) ) + 3/2*w(r)*w(r)*w(r)*J*J/(r*r*r);
}

float df(float r, float J) {
	float h = 0.01;
	return (f(r+h,J) - f(r,J))/h;
}

float df2(float r, float rp, float J) {
	return 2*sqrt( rp*rp + f(r,J)*f(r,J)*df(r,J)*df(r,J) );
}

vec4 illusion(vec3 pos, float t) {
	float h = 0., ht = 0.;
	vec2 rv = (player-bh).xz;
	float r = length(rv);

	float theta = 0; //atan(rv.y, rv.x);
	float eps = 0.1;

	vec2 ur = normalize(rv);
	vec2 ut = vec2(-ur.y, ur.x);

	vec3 v3 = normalize(pos - player);
	vec2 v = vec2(length(v3.xy), v3.z);

	float rp = dot(v,ur);
	float thetap = dot(v,ut)/r;
	float J = r*r/w(r)*thetap;

	while (ht < t) {
		// float delta = df2(r,rp,J);
		// h = min( 1 , max(  t/50. , 2*sqrt(eps/delta) ) );
		h = min( t - ht , t/100);

		if (r < rs && bh_active) 
			return bh.xyxy;
			// return vec2(r*cos(theta), r*sin(theta))+bh;
		rp += h*f(r,J);
		r += h*rp;
		theta += h*J*w(r)/(r*r);
		ht += h;
	}
	if (bh_active) {
		// return rotate(vec3(r*cos(theta), r*sin(theta), 0)+bh, vec3(0,0,-1), phi);
		// return rotate(vec3(rp*cos(theta) - J*w(r)/r*sin(theta), 0, rp*sin(theta) + J*w(r)/r*cos(theta))
		// 	, vec3(0,0,1), phi);
		return vec4(r*cos(theta), r*sin(theta), 
			rp*cos(theta) - J*w(r)/r*sin(theta), rp*sin(theta) + J*w(r)/r*cos(theta));
	}
	else{
		return pos.xyxy;
	}

}
void main()
{
	// float retard = length(position-player.xy);
	vec3 ray = vec3(position.x, position.y, -0.5);
	float phi = atan(ray.y, ray.x);

	if (bh_active) {
		vec4 ill = illusion(ray,10);
		if (length(ill.xy) < rs) {
			FragColor = vec4(0,0,0,1);
		}
		// if (length(ray - bh) < rs) {
		// 	float f = length(ray - bh) / rs;
		// 	f = min(1,-0.1*log(1-f)*(f));
		// 	FragColor = vec4(f,f,f,1);
		// }
		// else {
		vec3 dir =  rotate(vec3(ill.z, 0, ill.w), vec3(0,0,1), phi);
		FragColor = vec4(colorAt(dir,0));
	// }
	}
	else {
		FragColor = vec4(colorAt(ray,0));
	}
}