rm ./blackHole
g++ src/*.cpp -o blackHole -std=c++11 -Iinclude -lglfw3 -lGL -lX11 -lpthread -lXrandr -lXi -ldl -lXxf86vm -lXcursor -lXinerama
