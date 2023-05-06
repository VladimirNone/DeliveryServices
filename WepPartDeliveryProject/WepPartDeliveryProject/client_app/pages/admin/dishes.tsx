import DishAdminCard from "@/components/cards/DishAdminCard";
import UserCard from "@/components/cards/UserCard";
import PanelToHandleDishes from "@/components/PanelToHandleDishes";
import PanelToHandleUsers from "@/components/PanelToHandleUsers";
import ClientLayout from "@/components/structure/ClientLayout";
import { GetStaticProps } from "next";
import { FC, useEffect, useState } from "react";
import { Button } from "react-bootstrap";

export const getStaticProps: GetStaticProps = async () => {
    const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categories = await resp.json() as categoryItem[];

    return {
        props: {
            categories,
        }
    }
}

const Dishes: FC<{categories:categoryItem[]}> = ({ categories}) => {
    const [dishes, setDishes] = useState<dishAdminInfo[]>([]);

    //нулевая страница загружается при переходе на страницу
    const [page, setPage] = useState(0);
    const [pageEnded, setPageEnded] = useState(true);

    const [searchText, setSearchText] = useState<string>("");
    
    const handleChangeSearchedText = (searchedText:string) => {
        setSearchText(searchedText);
        setPage(0);
        setDishes([]);
    }

    const handleShowMoreDishes = async () => {
        const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/admin/getDishes?${searchText != "" ? "searchText=" + searchText + "&" : ""}page=${page}`, {
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
            }
        });
        const loadedData = await resp.json() as {dishes: dishAdminInfo[], pageEnded: boolean};
    
        if(resp.ok){
            setPage(page + 1);
            setDishes(dishes.concat(loadedData.dishes));
            setPageEnded(loadedData.pageEnded);
        }
        else{
            setPageEnded(true);
        }
    }

    const handleChangeDeleteStatusOfDish = async (dishId:string) => {
        const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/admin/changeDeleteStatusOfDish`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify({id: dishId})
        });
        if(resp1.ok){
            const respData = await resp1.json() as boolean;

            setDishes(prevDishes => {
                const changedDish:dishAdminInfo | undefined = prevDishes.find(el => el.id == dishId );
                if(changedDish != undefined)
                    changedDish.isDeleted = respData;

                return prevDishes.slice();
            });
        }
    }

    const handleChangeVisibleStatusOfDish = async (dishId:string) => {
        console.log(dishId);
        const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/admin/changeVisibleStatusOfDish`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify({id: dishId})
        });
        if(resp1.ok){
            const respData = await resp1.json() as boolean;

            setDishes(prevDishes => {
                const changedDish:dishAdminInfo | undefined = prevDishes.find(el => el.id == dishId );
                if(changedDish != undefined)
                    changedDish.isAvailableForUser = respData;

                return prevDishes.slice();
            });
        }
    }

    useEffect(()=>{
        if(page == 0){
            handleShowMoreDishes();
        }
    },[page, searchText]);

    return (
        <ClientLayout categories={categories}>
            <PanelToHandleDishes changeSearchedText={handleChangeSearchedText}/>
            {dishes.map((dish, i)=> <DishAdminCard key={i} {...dish} changeDeletedState={handleChangeDeleteStatusOfDish} changeVisibleState={handleChangeVisibleStatusOfDish}/>)}
            {!pageEnded && (
                <div>
                    <Button className='btn btn-primary w-100 mt-2' onClick={handleShowMoreDishes}>
                        Показать больше
                    </Button>
                </div>)
            }
        </ClientLayout>
    );
}

export default Dishes;