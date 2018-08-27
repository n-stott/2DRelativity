#version 330 core
out vec4 FragColor;
varying vec2 position;
uniform vec3 ourColor;
uniform float time;
uniform float c = 0.1;

uniform vec3 bh = vec3(0,0.,0);
uniform vec2 player = vec2(-0.3,0.);
uniform float rs = 0.2;

uniform bool bh_active = false;

vec4 colorAt(vec2 pos, float t) {
	float d;
	d =  time - t;
	d = atan(pos.y, pos.x);
	if (length(pos - bh.xy) <= rs && bh_active) {
		float f = 0; //1 - length(pos - bh) / rs;
		f = sqrt(f);
		// f = 0;
		return vec4(f,f,f,1);
	}
	else {
		// d = 3*pos.x + 2*time;
		d += time;
		// d = atan((player-pos).y, (player-pos).x);
		return vec4(0.5*sin(d+2) + 0.5, 0.5*sin(d+4) + 0.5, 0.5*sin(d) + 0.5, 1.0f);
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

vec2 illusion(vec2 pos, float t) {
	float h = 0., ht = 0.;
	vec2 rv = player-bh.xy;
	float r = length(rv);

	float theta = atan(rv.y, rv.x);
	float eps = 0.1;

	vec2 ur = normalize(rv);
	vec2 ut = vec2(-ur.y, ur.x);

	vec2 v = normalize(pos - player);

	float rp = dot(v,ur);
	float thetap = dot(v,ut)/r;
	float J = r*r/w(r)*thetap;

	while (ht < t) {
		// float delta = df2(r,rp,J);
		// h = min( 1 , max(  t/50. , 2*sqrt(eps/delta) ) );
		h = min( t - ht , t/100);

		if (r < rs && bh_active) 
			return bh.xy;
			// return vec2(r*cos(theta), r*sin(theta))+bh;
		rp += h*f(r,J);
		r += h*rp;
		theta += h*J*w(r)/(r*r);
		ht += h;
	}
	if (bh_active) {
		return vec2(rp*cos(theta) - J*w(r)/r*sin(theta), rp*sin(theta) + J*w(r)/r*cos(theta));
		return vec2(r*cos(theta), r*sin(theta))+bh.xy;
	}
	else{
		return pos;
	}

}
void main()
{
	float retard = length(position-player);
	vec2 ill = illusion(position,retard);
	if (length(position - bh.xy) < rs) {
		float f = length(position - bh.xy) / rs;
		f = min(1,-0.1*log(1-f)*(f));
		FragColor = vec4(f,f,f,1);
	}
	else {
	    FragColor = vec4(colorAt(ill,length(ill-player)/c));
	}
}