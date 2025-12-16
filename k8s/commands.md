### Create a Kind Cluster
```sh
kind create cluster --config=k8s/cluster-config.yaml

kubectl cluster-info --context kind-dotnet-ai-cluster
```

### List Deployments
```sh
kubectl get deployments
```

### View cluster events
```sh
kubectl get events
```

### Create Namespace
```sh
kubectl apply -f k8s/dotnet-ai-namespace.yaml
```

### View Namespaces
```sh
kubectl get namespaces
```