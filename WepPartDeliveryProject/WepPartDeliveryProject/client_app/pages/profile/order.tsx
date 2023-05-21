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
    const [orderInfo, setOrderInfo] = useState<orderInfo>({ order: {id:"", deliveryAddress: "", price:0, sumWeight:0, phoneNumber: '', DeleteOrder: ()=>{}, MoveOrderToNextStage: ()=>{}, MoveOrderToPreviousStage: ()=>{} }, orderedDishes: []});
    const orderId = router.query["orderId"];

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

            if(resp.ok){
                const orderInfo = await resp.json() as orderInfo;
                setOrderInfo(orderInfo);
            }
            else{
                router.push("/profile/orderStory");
                //alert(await resp.text());
            }
        }
        fetchData();
    }, [orderId, router]);

    return (
        <>
            <ClientLayout categories={categories}>
                <OrderCard {...orderInfo.order} canWriteReview={orderInfo.order.clientRating == null}/>
                {orderInfo.order.clientRating != null && <h3>Оценка клиента: {orderInfo.order.clientRating}</h3>}
                {orderInfo.order.review != null && <h3>Отзыв клиента: {orderInfo.order.review}</h3>}
                <Row className="mt-3 ">
                    <Col >
                        <h3>Список блюд из заказа: </h3>
                    </Col>
                </Row>
                {orderInfo.orderedDishes.map((dish, i) => {
                    return (<DishOrderCard key={i} {...dish} orderId={orderInfo.order.id} />)
                })}
                
            </ClientLayout>
        </>
    );
}

export default Order;