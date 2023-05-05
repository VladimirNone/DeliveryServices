import OrderCard from "@/components/cards/OrderCard";
import ClientLayout from "@/components/structure/ClientLayout";
import { GetStaticProps } from "next";
import { FC, useEffect, useState } from "react";

export const getStaticProps: GetStaticProps = async () => {
    const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categories = await resp.json() as categoryItem[];

    return {
        props: {
            categories
        }
    }
}

const OrderStory: FC<{ categories: categoryItem[] }> = ({ categories }) => {
    const [orders, setOrders] = useState<orderCardInfo[]>([]);

    const handleDeleteItem = async (orderId:string) => {
        setOrders(prevOrders => prevOrders.filter(el => el.id != orderId ));

        const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/cancelOrder`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify({orderId})
        });
    }

    useEffect(() => {
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/getClientOrders`, {
                credentials: 'include',
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                },
            });
            const ordersInfo = await resp.json() as orderCardInfo[];
            setOrders(ordersInfo);
        }
        fetchData();
    }, []);

    return (
        <ClientLayout categories={categories}>
            {orders.map((order, i)=> <OrderCard key={i} {...order} DeleteOrder={handleDeleteItem}/>)}
        </ClientLayout>
    );
}

export default OrderStory;