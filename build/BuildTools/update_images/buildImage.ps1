Invoke-Expression -Command (aws ecr get-login --no-include-email --profile vss-grant --region us-west-2)
docker build -t 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:updateimages-dind ./
docker push 276986344560.dkr.ecr.us-west-2.amazonaws.com/base-images:updateimages-dind