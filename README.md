input format
starts with single letter 'd';
followed by the number of nodes n and the number of edges m
for each of the m edges, we have
source node ID s, an unsigned 32-bit integer, 0 <= s < n;
target node ID t, an unsigned 32-bit integer, 0 <= t < n;
edge weight w, an unsigned 32-bit integer; note that the length of the longest shortest path must fit into a 32-bit integer
the direction d:
0 = open in both directions
1 = open only in forward direction (from s to t)
2 = open only in backward direction (from t to s)
3 = closed
