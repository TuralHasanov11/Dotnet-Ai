### Create a Kind Cluster
```sh
kind create cluster --config=k8s/cluster-config.yaml

kubectl cluster-info --context kind-dotnet-ai-cluster
```


### View cluster events
```sh
kubectl get events
```

### Create Namespace
```sh
kubectl apply -f k8s/dotnet-ai-namespace.yaml
```

### Start running namespace
```sh
kubectl config set-context --current --namespace=dotnet-ai
```

### View Namespaces
```sh
kubectl get namespaces
```

### List Deployments
```sh
kubectl get deployments -n dotnet-ai
```

### View Services
```sh
kubectl get services -n dotnet-ai
```

### Configure Service Accounts
```sh
kubectl apply -f k8s/agent-framework-quick-start-service-account.yaml
```

### Configure Role to allow reading ConfigMaps
```sh
kubectl apply -f k8s/configmap-reader-role.yaml
kubectl apply -f k8s/configmap-reader-role-binding.yaml
```

### Deploy Redis to Kubernetes
```sh
kubectl apply -f k8s/redis-configmap.yaml
kubectl apply -f k8s/redis-service.yaml
kubectl exec -it pod/redis -- redis-cli
```

### Deploy Qdrant to Kubernetes
```sh
kubectl apply -f k8s/qdrant-pvc.yaml
kubectl apply -f k8s/qdrant-service.yaml
```

### Deploy Agent Framework Quick Start to Kubernetes
```sh
kubectl apply -f k8s/agent-framework-quick-start-config.yaml
kubectl apply -f k8s/agent-framework-quick-start-service.yaml
```

### Make a curl request to the Agent Framework Quick Start API
```sh
kubectl get services/agent-framework-quick-start -o go-template='{{(index .spec.ports 0).nodePort}}' -n dotnet-ai
curl http://localhost:5200/health
``` 

### Deploy Kubernetes Dashboard using Helm
```sh
# Add kubernetes-dashboard repository
helm repo add kubernetes-dashboard https://kubernetes.github.io/dashboard/
# Deploy a Helm Release named "kubernetes-dashboard" using the kubernetes-dashboard chart
helm upgrade --install kubernetes-dashboard kubernetes-dashboard/kubernetes-dashboard --create-namespace --namespace kubernetes-dashboard

# Kubectl will make Dashboard available at https://localhost:8443
kubectl -n kubernetes-dashboard port-forward svc/kubernetes-dashboard-kong-proxy 8443:443
```