import requests
from bs4 import BeautifulSoup
import pandas as pd
from datetime import datetime

url = "https://batdongsan.com.vn/du-an-can-ho-chung-cu"

headers = {
    "User-Agent":"Mozilla/5.0"
}

response = requests.get(url, headers=headers)

soup = BeautifulSoup(response.text,"html.parser")

projects=[]

items=soup.select(".project-item")

for item in items:
    try:
        name=item.select_one(".project-name").text.strip()
        price=item.select_one(".price").text.strip()

        projects.append({
            "name":name,
            "price":price,
            "date":datetime.now()
        })

    except:
        pass

df=pd.DataFrame(projects)

df.to_csv("realestate_data.csv",index=False)