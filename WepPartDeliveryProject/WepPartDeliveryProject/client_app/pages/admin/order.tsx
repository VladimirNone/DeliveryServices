import DishOrderCard from "@/components/cards/DishOrderCard";
import OrderCard from "@/components/cards/OrderCard";
import ClientLayout from "@/components/structure/ClientLayout";
import { GetStaticProps } from "next";
import { useRouter } from "next/router";
import { FC, useEffect, useState } from "react";
import { Col, Row } from "react-bootstrap";

export const getStaticProps: GetStaticProps = async () => {
    const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/main/getCategoriesList`);
    const categories = await resp.json() as categoryItem[];

    return {
        props: {
            categories
        }
    }
}

const Order: FC<{ categories: categoryItem[]}> = ({ categories }) => {
    const router = useRouter();
    const [orderInfo, setOrderInfo] = useState<orderInfo>({ order: {id:"", deliveryAddress: "", price:0, sumWeight:0, DeleteOrder: ()=>{}, MoveOrderToNextStage: ()=>{}, MoveOrderToPreviousStage: ()=>{} }, orderedDishes: []});
    const orderId = router.query["orderId"];

    const handleDeleteOrder = async () => {
        const resp1 = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/cancelOrder`, {
            method: "POST",
            headers: {
                'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                'Content-Type': 'application/json;charset=utf-8',
            },
            body: JSON.stringify({ orderId })
        });

        if (resp1.ok) {
            router.push("/profile/orderStory");
        }
    }

    const handleDeleteItemDish = async (dishId: string) => {
        const dishWillDelete = orderInfo.orderedDishes.find(el => el.dishInfo.id == dishId)
        if (dishWillDelete != null) {
            setOrderInfo(prevOrderInfo => {
                prevOrderInfo.orderedDishes = prevOrderInfo.orderedDishes.filter(el => el.dishInfo.id != dishId);
                prevOrderInfo.order.price -= dishWillDelete.count*dishWillDelete.dishInfo.price;
                return { ...prevOrderInfo };
            });

            const response = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/cancelOrderedDish`, {
                method: "POST",
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                    'Content-Type': 'application/json;charset=utf-8',
                },
                body: JSON.stringify({ orderId, dishId })
            });

            if (orderInfo.orderedDishes.length == 0) {
                await handleDeleteOrder();
            }
        }
    }

    useEffect(() => {
        if(orderId == undefined || orderId == null)
            return;
        const fetchData = async () => {
            const resp = await fetch(`${process.env.NEXT_PUBLIC_HOME_API}/order/getOrder/${orderId}`, {
                credentials: 'include',
                headers: {
                    'Authorization': 'Bearer ' + localStorage.getItem("jwtToken"),
                }, 
            });
            
            const orderInfo = await resp.json() as orderInfo;
            setOrderInfo(orderInfo);
        }
        fetchData();
    }, [orderId, router]);

    return (
        <>
            <ClientLayout categories={categories}>
                <OrderCard {...orderInfo.order} DeleteOrder={handleDeleteOrder}/>
                <Row className="mt-3 ">
                    <Col >
                        <h3>Список блюд из заказа: </h3>
                    </Col>
                </Row>
                {orderInfo.orderedDishes.map((dish, i) => {
                    dish.dishInfo.DeleteCardFromList = handleDeleteItemDish
                    return (<DishOrderCard key={i} {...dish} orderId={orderInfo.order.id} />)
                })}
                
            </ClientLayout>
        </>
    );
}

export default Order;