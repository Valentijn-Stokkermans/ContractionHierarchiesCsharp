import pandas as pd
# use https://github.com/Tristramg/osm4routing2 
# to get an edges.csv and nodes.csv from an osm.pbf file

# read edges.csv into a pandas DataFrame
df = pd.read_csv('edges.csv')

# map the source and target ids
id_map = {}
new_id = 0
for old_id in set(df['source']).union(set(df['target'])):
    id_map[old_id] = new_id
    new_id += 1
df['new_source'] = df['source'].map(id_map)
df['new_target'] = df['target'].map(id_map)

# drop rows where car_forward is Forbidden and car_backward is not Forbidden
df = df[(df['car_forward'] != 'Forbidden') | (df['car_backward'] == 'Forbidden')]

# calculate weight based on speed limit
speed_limits = {"motorway": 27.77, "trunk": 27.77, "primary": 22.16, "secondary": 13.85, "tertiary": 8.31, "residential": 5.54}
df['weight'] = df.apply(lambda row: row['length'] / speed_limits.get(row['car_forward'], 50), axis=1)

# create new rows for car_backward
backwards_df = df[(df['car_backward'] != 'Forbidden')]
backwards_df = backwards_df.rename(columns={'source': 'target', 'target': 'source'})
backwards_df['new_source'] = backwards_df['source'].map(id_map)
backwards_df['new_target'] = backwards_df['target'].map(id_map)
backwards_df['weight'] = backwards_df.apply(lambda row: row['length'] / speed_limits.get(row['car_backward'], 50), axis=1)

# concatenate df and backwards_df
df = pd.concat([df, backwards_df])

# drop unnecessary columns
df = df[['new_source', 'new_target', 'weight','source', 'target']]

# read nodes.csv into a pandas DataFrame
nodes_df = pd.read_csv('nodes.csv')

# map the ids
nodes_df['new_id'] = nodes_df['id'].map(id_map)

# write the modified DataFrames to new csv files
df.to_csv('new_edges.csv', index=False)
nodes_df.to_csv('new_nodes.csv', index=False)