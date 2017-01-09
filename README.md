![Alt text](Detection3d.PNG?raw=true "Web Portal in Angular and MVC showing 3d plot of Kmeans")

I.	Introduction
Anomaly Detection API is a project that clusters given data based on their similarities. This project uses the K-mean algorithm to detect similarity which results on depending on Euclidean distances and thus we can only deal with numeric data. This Project has two main parts:
-	Part 1: take samples and cluster them. In this phase the program will learn about the clusters and thus can be considered as the learning phase.
-	Part 2: based on the clustering results that we obtained from part 1, the program should detect to which cluster a new given sample belongs if any.

II.	WebApI Objectives
To fulfill the tasks required and mentioned in the introduction we were asked to implement the following features:
-	Import new Data and cluster it.
-	Check the sample whether it is an outlier or belongs to a cluster
-	Import and append new Data to an existing clustered data. Here only the accepted samples (not outliers) are added and the rest is ignored.
-	Calculate Statics about the clusters.
-	Find a method to recommend the number of clusters for a given data set.

III. WebPortal Objective
-	In this project, we were supposed to build a web portal which could provide interesting cluster statistics and Graphical representation of the raw data presented to the API and display the K-means clustering results and the original (raw) data in 2D and 3D forms. 
-	Project Requirement 
-	Web Portal for Kmeans using Web API
-	Implement a simple portal which provides interesting cluster information.
a.	Cluster Statistics 
Methods which return some usable cluster statistical data. For example
i.	Distance to farther sample from cluster centroid. 
ii.	Distance to next nearest cluster. 
iii.	Distance between nearest samples of nearest clusters.
b.	Graphical representation of Cluster-Centroids, Farthest Sample from Centroid Nearest Clusters, nearest samples from two clusters.
c.	Show data in 2D and 3D by freezing remaining scalars.  


Please Read Pdfs included for complete Details on the Implementation of the Project
